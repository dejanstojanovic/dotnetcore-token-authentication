Add-Migration InitialCreate -Project Sample.Core.Identity.Data  -Context ApplicationUserDbContext
Update-Database -Project Sample.Core.Identity.Data -Context ApplicationUserDbContext
Remove-Migration -Project Sample.Core.Identity.Data -Context ApplicationUserDbContext