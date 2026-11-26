#!/usr/bin/env bash
set -euo pipefail

export SQLCMDPASSWORD="${MSSQL_SA_PASSWORD:?Falta MSSQL_SA_PASSWORD}"
sqlcmd=/opt/mssql-tools18/bin/sqlcmd

schema_ready=$("$sqlcmd" -S db -U sa -C -b -h -1 -W -Q "SET NOCOUNT ON;
IF DB_ID(N'VeterinariaS14DB') IS NULL SELECT 0;
ELSE EXEC(N'USE VeterinariaS14DB;
SELECT CASE WHEN OBJECT_ID(N''dbo.Propietarios'', N''U'') IS NOT NULL
AND OBJECT_ID(N''dbo.Mascotas'', N''U'') IS NOT NULL THEN 1 ELSE 0 END;');" | tr -d '[:space:]')

if [[ "$schema_ready" == "1" ]]; then
    "$sqlcmd" -S db -U sa -C -b -i /init/Usuarios.sql
    echo 'La base de veterinaria ya estaba inicializada; se agrego Usuarios si faltaba.'
    exit 0
fi

"$sqlcmd" -S db -U sa -C -b -i /init/Veterinaria.sql
"$sqlcmd" -S db -U sa -C -b -i /init/Usuarios.sql
echo 'Base de veterinaria creada con los datos de ejemplo y las cuentas de prueba.'
