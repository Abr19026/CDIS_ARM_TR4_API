/*
Actualizar modelo EFCORE con
dotnet ef dbcontext scaffold "Server=localhost\SQLExpress;Database=Bank;Trusted_connection=true;Encrypt=False" Microsoft.EntityFrameworkCore.SqlServer --context-dir .\Data --output-dir .\Data\BankModels --force
Es necesario el 
dotnet tool install --global dotnet-ef --version 7.* 
*/
USE Bank;
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

INSERT INTO Administrator (Name, PhoneNumber, Email, Pwd, AdminType) 
VALUES ('Abraham', '83333333333', 'abrahm@mail.com', '1234', 'Super'),
('José', '8444444444', 'josito@mail.com', 'abcde', 'Viewer')

GO
ALTER TABLE Client
ADD Pwd VARCHAR(40)

GO
UPDATE Client SET Pwd = "passana" WHERE ID = 2

GO
ALTER TABLE BankTransaction
ALTER COLUMN AccountID INT NULL

ALTER TABLE BankTransaction
DROP CONSTRAINT FK__BankTrans__Accou__571DF1D5

ALTER TABLE BankTransaction
ADD CONSTRAINT PK_BankTransaction_AccountID 
	FOREIGN KEY (AccountID)
	REFERENCES Account(Id)
	ON DELETE SET NULL
