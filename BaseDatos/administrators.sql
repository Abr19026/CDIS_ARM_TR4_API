Use Bank;
CREATE TABLE Administrator
(
	ID INT PRIMARY KEY IDENTITY(1,1),
	Name VarChar(200) NOT NULL,
	PhoneNumber VarChar(40) NOT NULL,
	Email VarChar(50) NOT NULL,
	Pwd VarChar(50) NOT NULL,
	AdminType VarChar(30) NOT NULL,
	RegDate DATETIME NOT NULL DEFAULT GETDATE(),
)
-- Actualizar modelo EFCORE con
-- dotnet ef dbcontext scaffold "Server=localhost\SQLExpress;Database=Bank;Trusted_connection=true;Encrypt=False" Microsoft.EntityFrameworkCore.SqlServer --context-dir .\Data --output-dir .\Data\BankModels --force

insert into Administrator (name, PhoneNumber, Email, Pwd, AdminType) values
("Abraham", "83333333333", "abrahm@mail.com", "1234", "Super"),
("José", "8444444444", "josito@mail.com", "abcde", "Viewer"),
