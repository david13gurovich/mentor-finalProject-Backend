MariaDB - our DataBase

download -https://mariadb.org/

packages - type in Package Manager Console, when "repos" or where your migration is stored is chosen:
Install-Package Pomelo.EntityFrameworkCore.MySQL -Version 6.0.1
Install-Package Microsoft.EntityFrameworkcore.Tools -version 6.0.1

VisualStudio -> search -> package maneger -> ( a console window open) -> write: Add-Migration "nameOfYpurChoise"  (to remember - compilation)->
-> write: Update-DataBase  (to remember - sync)  -----  we got DB

how to play with it: open prompt - search in windown : "MySQL Client (MariaDB ... )" -> then password
use PomeloDB (name of the db);   - to open DB
(open the table "user") SELECT * FROM user;

if shows - already included "something":
- then  delete all migration files
- then go to maria prompt and write: drop database "database name"

if you already have db that not update or you want to add model (table): write "drop databse PomeloDB;" in the prompt , delete all migration files and Add-Migration again (check before that you don't have errors if you delete the migration folder for exmple).
davidgurovich 

