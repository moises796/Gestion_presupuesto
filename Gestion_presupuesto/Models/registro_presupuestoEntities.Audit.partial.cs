using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Gestion_presupuesto.Models
{
	public partial class registro_presupuestoEntities : DbContext
	{
		[ThreadStatic] private static bool _inAuditSave;

		public override int SaveChanges()
		{
			if (_inAuditSave) return base.SaveChanges();

			var staged = BuildAuditStaging(out var now, out var user);
			var rows = base.SaveChanges(); // aquí ya se generan las PK de Added

			// Completa RowId de los Added y persiste los logs
			_inAuditSave = true;
			try
			{
				FinalizeRowIds(staged);
				PersistAudit(staged, user, now);
			}
			finally { _inAuditSave = false; }

			return rows;
		}

		public override Task<int> SaveChangesAsync()
			=> SaveChangesAsync(CancellationToken.None);

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
		{
			if (_inAuditSave) return await base.SaveChangesAsync(cancellationToken);

			var staged = BuildAuditStaging(out var now, out var user);
			var rows = await base.SaveChangesAsync(cancellationToken);

			_inAuditSave = true;
			try
			{
				FinalizeRowIds(staged);
				PersistAudit(staged, user, now);
			}
			finally { _inAuditSave = false; }

			return rows;
		}

		// ========= Núcleo de auditoría =========

		private class AuditRow
		{
			public string TableName;
			public string Operation; // I/U/D
			public string RowId;     // k=v;k2=v2
			public string BeforeJson;
			public string AfterJson;
			public DbEntityEntry Entry; // para completar PK tras SaveChanges en Added
			public bool NeedsKey;
		}

		private List<AuditRow> BuildAuditStaging(out DateTime now, out string user)
		{
			this.ChangeTracker.DetectChanges();

			now = DateTime.Now;
			user = GetUserNameOrAnon();

			var entries = this.ChangeTracker.Entries()
				.Where(e => e.Entity != null &&
						   (e.State == EntityState.Added
						 || e.State == EntityState.Modified
						 || e.State == EntityState.Deleted))
				.ToList();

			var staged = new List<AuditRow>();
			var objCtx = ((IObjectContextAdapter)this).ObjectContext;

			foreach (var e in entries)
			{
				var clrType = ObjectContext.GetObjectType(e.Entity.GetType());
				var tableName = clrType.Name;

				// Evita auditar la propia tabla de logs si la mapearas al .edmx
				if (tableName.Equals("logs_tb", StringComparison.OrdinalIgnoreCase))
					continue;

				var op = e.State == EntityState.Added ? "I"
					   : e.State == EntityState.Modified ? "U" : "D";

				// Armamos before/after (solo campos “útiles”, ver filtro)
				var beforeDict = (op == "I") ? null : ValuesToDict(e.OriginalValues);
				var afterDict = (op == "D") ? null : ValuesToDict(e.CurrentValues);

				// Si es UPDATE, opcional: deja solo los campos que cambiaron
				if (op == "U")
				{
					FilterOnlyChanges(e, beforeDict, afterDict);
				}

				var row = new AuditRow
				{
					TableName = tableName,
					Operation = op,
					RowId = GetRowId(e, objCtx, preferOriginal: op != "I"),
					BeforeJson = beforeDict != null ? ToJson(beforeDict) : null,
					AfterJson = afterDict != null ? ToJson(afterDict) : null,
					Entry = e,
					NeedsKey = (op == "I") && string.IsNullOrEmpty(null) // siempre true para Added
				};

				staged.Add(row);
			}

			return staged;
		}

		private void FinalizeRowIds(List<AuditRow> staged)
		{
			var objCtx = ((IObjectContextAdapter)this).ObjectContext;
			foreach (var r in staged.Where(x => x.NeedsKey))
			{
				r.RowId = GetRowId(r.Entry, objCtx, preferOriginal: false);
			}
		}

		private void PersistAudit(List<AuditRow> staged, string user, DateTime now)
		{
			foreach (var r in staged)
			{
				this.Database.ExecuteSqlCommand(
					@"INSERT INTO auditoria.logs_tb (TableName, Operation, RowId, UserName, AtUtc, BeforeJson, AfterJson)
                      VALUES (@p0,@p1,@p2,@p3,@p4,@p5,@p6);",
					r.TableName, r.Operation, r.RowId ?? "", user, now, (object)r.BeforeJson ?? DBNull.Value, (object)r.AfterJson ?? DBNull.Value
				);
			}
		}

		// ========= Helpers =========

		private static readonly HashSet<string> IgnoredProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
            // quítalas si quieres ver también las columnas de sellado
            "CreatedAt","CreatedBy","UpdatedAt","UpdatedBy","IsDeleted","DeletedAt","DeletedBy"
		};

		private static Dictionary<string, object> ValuesToDict(DbPropertyValues vals)
		{
			var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			foreach (var name in vals.PropertyNames)
			{
				if (IgnoredProps.Contains(name)) continue;
				var v = vals[name];
				if (v is DbPropertyValues nested)
					map[name] = ValuesToDict(nested); // complex types
				else
					map[name] = v;
			}
			return map;
		}

		private static void FilterOnlyChanges(DbEntityEntry e,
			Dictionary<string, object> before, Dictionary<string, object> after)
		{
			if (before == null || after == null) return;

			var changed = new HashSet<string>(
				e.CurrentValues.PropertyNames
				 .Where(n => !IgnoredProps.Contains(n))
				 .Where(n => !object.Equals(e.OriginalValues[n], e.CurrentValues[n])),
				StringComparer.OrdinalIgnoreCase
			);

			// deja solo las cambiadas
			foreach (var key in before.Keys.ToList())
				if (!changed.Contains(key)) before.Remove(key);
			foreach (var key in after.Keys.ToList())
				if (!changed.Contains(key)) after.Remove(key);
		}

		private static string ToJson(object obj)
		{
			var settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				DateFormatHandling = DateFormatHandling.IsoDateFormat
			};
			return JsonConvert.SerializeObject(obj, settings);
		}

		private static string GetRowId(DbEntityEntry e, ObjectContext objCtx, bool preferOriginal)
		{
			// Obtiene los nombres de las PK desde el metamodelo
			var entityType = ObjectContext.GetObjectType(e.Entity.GetType());
			var container = objCtx.MetadataWorkspace
								   .GetEntityContainer(objCtx.DefaultContainerName, DataSpace.CSpace);
			var set = container.BaseEntitySets
							   .OfType<EntitySet>()
							   .FirstOrDefault(s => s.ElementType.Name == entityType.Name);

			var keys = set?.ElementType.KeyMembers.Select(k => k.Name).ToList()
					   ?? new List<string> { "Id" }; // fallback

			var src = preferOriginal ? e.OriginalValues : e.CurrentValues;

			var parts = new List<string>();
			foreach (var k in keys)
			{
				object val = null;
				try
				{
					val = src[k];
				}
				catch
				{
					// si no está en src (Added/Deleted), intenta en el otro
					var alt = preferOriginal ? e.CurrentValues : e.OriginalValues;
					try { val = alt[k]; } catch { /* ignore */ }
				}
				parts.Add($"{k}={val}");
			}
			return string.Join(";", parts);
		}

		private static string GetUserNameOrAnon()
		{
			try
			{
				var principal = HttpContext.Current?.User as ClaimsPrincipal;
				if (principal?.Identity?.IsAuthenticated == true)
				{
					var preferred = principal.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
					return preferred ?? principal.Identity.Name ?? "NO-USER";
				}
			}
			catch { }
			return "NO-USER";
		}
	}
}