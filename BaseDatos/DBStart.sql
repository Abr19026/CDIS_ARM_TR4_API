--CREATE DATABASE Bank;

USE Bank;
/*
USE Bank;
CREATE TABLE Client
(
    ID INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(200) NOT NULL,
    PhoneNumber VARCHAR(40) NOT NULL,
    Email VARCHAR(50),
    Balance DECIMAL(10,2)  
)
*/

/*
USE Bank;
ALTER TABLE Client
DROP COLUMN Balance

ALTER TABLE Client
ADD RegDate DATETIME DEFAULT GETDATE();

ALTER TABLE Client
ALTER COLUMN RegDate DATETIME NOT NULL;
*/

/*
USE Bank;

INSERT INTO Client (Name, PhoneNumber,Email)
VALUES 
('JOSE', '8123213312', 'pedro@gmail.com')

SELECT * FROM Client;
*/

/*
USE Bank;

INSERT INTO Client (Name, PhoneNumber,Email)
VALUES 
('Ana', '8143243312', 'ana@live.com'),
('Raúl', '432432432', 'raul3000@hotmail.com'),
('Carlos', '8765432135', 'ch31xm@yahoo.com'),
('Marta', '6424657657', 'marta@mail.com')

UPDATE Client SET Email = NULL
WHERE ID IN (4,6)

UPDATE Client SET Email = 'pedro@yahoo.com'
WHERE ID = 1

SELECT * FROM Client;
*/

/*
USE Bank;

DELETE FROM Client
WHERE Name = 'Marta'

SELECT * FROM Client;
*/

/*
USE Bank;

CREATE TABLE TransactionType
(
    ID INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(200) NOT NULL,
    RegDate DATETIME DEFAULT GETDATE(),
)

CREATE TABLE AccountType
(
    ID INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(200) NOT NULL,
    RegDate DATETIME DEFAULT GETDATE(),
)

CREATE TABLE Account
(
    ID INT PRIMARY KEY IDENTITY(1,1),
    AccountType INT NOT NULL FOREIGN KEY REFERENCES AccountType(ID),
    ClientID INT NOT NULL FOREIGN KEY REFERENCES Client(ID),
    Balance DECIMAL(10,2) NOT NULL,
    RegDate DATETIME NOT NULL DEFAULT GETDATE(),
)

CREATE TABLE BankTransaction
(
    ID INT PRIMARY KEY IDENTITY(1,1),
    AccountID INT NOT NULL FOREIGN KEY REFERENCES Account(ID),
    TransactionType INT NOT NULL FOREIGN KEY REFERENCES TransactionType(ID),
    Amount DECIMAL(10,2) NOT NULL,
    ExternalAccount INT NULL, -- Cuando es en retiro efectivo es nulo, no es fk porque puede ser de otro banco
    RegDate DATETIME NOT NULL DEFAULT GETDATE(),
)
*/

/*
USE Bank;

INSERT INTO AccountType (Name)
VALUES ('Personal'), ('Nómina'), ('Ahorro')

INSERT INTO TransactionType (Name)
VALUES  ('Depósito en Efectivo'), ('Retiro en Efectivo'),
        ('Depósito via Transferencia'), ('Retiro vía Transferencia');

SELECT * FROM AccountType
SELECT * FROM TransactionType;
*/

/*
USE Bank;
INSERT INTO Account (AccountType, ClientID, Balance)
VALUES  -- Ana
        (1, 4, 5000), -- Personal
        (2, 4, 10000), -- Nómina
        -- Raúl
        (1, 5, 3000), -- Personal
        (2, 5, 14000) -- Nómina

INSERT INTO BankTransaction(AccountID, TransactionType, Amount, ExternalAccount)
VALUES  (1,1,100,NULL),
        (1,3,200,123213),
        (3,1,100,NULL),
        (3,3,250,5423423)

SELECT * FROM Account
SELECT * FROM BankTransaction
*/

/*
USE Bank;
GO
SELECT a.ID, c.Name as 'Client Name', a.Balance, a.RegDate, at.Name as 'Tipo cuenta'
FROM Account a
INNER JOIN Client c ON a.ClientID = c.ID
INNER JOIN AccountType at ON a.AccountType = at.ID;

SELECT bt.ID, c.Name as 'Client Name', t.Name as 'Tipo transacción', bt.Amount, bt.ExternalAccount, bt.RegDate 
FROM BankTransaction bt
JOIN Account a ON bt.AccountID = a.ID
JOIN Client c on a.ClientID = c.ID
JOIN TransactionType t on bt.TransactionType = t.ID;
*/
/*
USE Bank;
GO
CREATE PROCEDURE SelectAccount
AS
    SELECT a.ID, c.Name as 'Client Name', a.Balance, a.RegDate, at.Name as 'Tipo cuenta'
    FROM Account a
    INNER JOIN Client c ON a.ClientID = c.ID
    INNER JOIN AccountType at ON a.AccountType = at.ID;
GO
*/
--EXEC SelectAccount
/*
USE Bank;
GO
ALTER PROCEDURE SelectAccount
    @ClientID INT = NULL
AS
    IF @ClientID IS NULL
    BEGIN
        SELECT a.ID, c.Name as 'Client Name', c.Email, a.Balance, a.RegDate, at.Name as 'Tipo cuenta'
        FROM Account a
        LEFT JOIN Client c ON a.ClientID = c.ID
        LEFT JOIN AccountType at ON a.AccountType = at.ID;
    END
    ELSE
    BEGIN
        SELECT a.ID, c.Name as 'Client Name', a.Balance, a.RegDate, at.Name as 'Tipo cuenta'
        FROM Account a
        JOIN Client c ON a.ClientID = c.ID
        LEFT JOIN AccountType at ON a.AccountType = at.ID
        WHERE a.ClientID = @ClientID;
    END
GO
*/

-- EXEC SelectAccount @ClientID = 4

/*
USE BANK;
GO

CREATE PROCEDURE InsertClient
    @Name VARCHAR(200),
    @PhoneNumber VARCHAR(40),
    @Email VARCHAR(50) = NULL
AS
    INSERT INTO Client (Name, PhoneNumber, Email)
    VALUES (@Name, @PhoneNumber, @Email);
GO

EXEC InsertClient @Name = 'Jacinto', @PhoneNumber = '231432432432'
EXEC InsertClient @Name = 'Jonathan', @PhoneNumber = '654654654', @Email = 'jon@gmail.com'
*/

/*
USE Bank;
GO

-- Crea cuenta automaticamente para cliente creado
CREATE TRIGGER ClientAfterInsert
ON Client
AFTER INSERT
AS
    DECLARE @NewClientID INT 
    SET @NewClientID = (SELECT ID FROM inserted);

    INSERT INTO Account (AccountType, ClientID, Balance)
    VALUES (1, @NewClientID, 0)
GO

EXEC SelectAccount
EXEC InsertClient @Name = 'Alex', @PhoneNumber = '765432876'
EXEC SelectAccount

GO

ALTER TABLE Account
ALTER COLUMN ClientID INT NULL;

GO
*/

/*
USE Bank;
GO
-- Elimina referencia al cliente antes de eliminar al cliente
CREATE TRIGGER ClientInsteadOfDelete
ON Client
INSTEAD OF DELETE
AS
    DECLARE @DeletedID INT;
    SET @DeletedID = (SELECT ID FROM deleted);

    UPDATE Account
    SET ClientID = NULL
    WHERE ClientID = @DeletedID;

    DELETE FROM Client WHERE ID = @DeletedID
GO

SELECT * FROM Client;
EXEC SelectAccount;

DELETE FROM Client WHERE Name = 'Alex';

SELECT * FROM Client;
EXEC SelectAccount;
*/

/* USE Bank;
GO

CREATE PROCEDURE InsertBankTransaction
    @AccountID INT,
    @TransactionType INT,
    @Amount DECIMAL(10,2),
    @ExternalAccount INT = NULL
AS
    DECLARE @CurrentBalance DECIMAL(10,2), 
            @NewBalance DECIMAL(10,2);

    BEGIN TRANSACTION
    SET @CurrentBalance = (SELECT Balance FROM Account WHERE ID = @AccountID);

    -- Obtiene nuevo saldo
    IF @TransactionType = 2 OR @TransactionType = 4
        -- Retiros
        SET @NewBalance = @CurrentBalance - @Amount;
    ELSE
        -- Depósitos
        SET @NewBalance = @CurrentBalance + @Amount;

    -- Actualiza saldo
    UPDATE Account
    SET Balance = @NewBalance
    WHERE ID = @AccountID;

    -- Registra transacción
    INSERT INTO BankTransaction 
    (AccountID, TransactionType, Amount, ExternalAccount)
    VALUES
    (@AccountID, @TransactionType, @Amount, @ExternalAccount)
    
    -- Cancelar si nuevo balance es menor a 0
    IF @NewBalance >= 0
        COMMIT TRANSACTION
    ELSE
        ROLLBACK
GO */

/* -- Prueba 1. retiro de 1000 a la cuenta 1
EXEC SelectAccount

EXECUTE InsertBankTransaction
@AccountID = 1,
@TransactionType = 2,
@Amount = 1000;

Exec SelectAccount
-- Prueba 2 , Retiro muy grande a cuenta 1, no se realiza
EXECUTE InsertBankTransaction
@AccountID = 1,
@TransactionType = 2,
@Amount = 999999;

Exec SelectAccount */

/* USE Bank;

GO

CREATE PROCEDURE SelectClient
    @ClientID INT = NULL
AS
    IF @ClientID IS NULL
        SELECT * FROM Client
    ELSE
        SELECT * FROM Client
        WHERE ID = @ClientID
GO

ALTER PROCEDURE InsertClient
    @Name VARCHAR(200),
    @PhoneNumber VARCHAR(40),
    @Email VARCHAR(50) = NULL
AS
    DECLARE @ANTERIOR INT = NULL;
    SET @ANTERIOR = (SELECT ID FROM Client WHERE Email = @Email);
    IF @ANTERIOR IS NULL
    BEGIN
        INSERT INTO Client (Name, PhoneNumber, Email)
        VALUES (@Name, @PhoneNumber, @Email);
        RETURN 0
    END
    ELSE
    BEGIN
        PRINT 'Error: Email ya existe'
        RETURN 1
    END
GO

ALTER PROCEDURE InsertBankTransaction
    @AccountID INT,
    @TransactionType INT,
    @Amount DECIMAL(10,2),
    @ExternalAccount INT = NULL
AS
    DECLARE @CurrentBalance DECIMAL(10,2), 
            @NewBalance DECIMAL(10,2);

    BEGIN TRANSACTION
    SET @CurrentBalance = (SELECT Balance FROM Account WHERE ID = @AccountID);

    -- Obtiene nuevo saldo
    IF @TransactionType = 2 OR @TransactionType = 4
        -- Retiros
        SET @NewBalance = @CurrentBalance - @Amount;
    ELSE
        -- Depósitos
        SET @NewBalance = @CurrentBalance + @Amount;

    -- Actualiza saldo
    UPDATE Account
    SET Balance = @NewBalance
    WHERE ID = @AccountID;

    -- Registra transacción
    INSERT INTO BankTransaction 
    (AccountID, TransactionType, Amount, ExternalAccount)
    VALUES
    (@AccountID, @TransactionType, @Amount, @ExternalAccount)
    
    -- Cancelar si nuevo balance es menor a 0
    IF @NewBalance >= 0
        COMMIT TRANSACTION
    ELSE
    BEGIN  
        ROLLBACK
        PRINT 'No hay saldo suficiente para completar la transacción'
		RETURN 1
    END
GO */

/*
-- Probando SelectClient
EXEC SelectClient;
EXEC SelectClient @ClientID=5
-- Probando Insert client con repetido
EXEC InsertClient @Name = 'John', @PhoneNumber = '4324323', @Email = 'john@gmail.com'
EXEC InsertClient @Name = 'John', @PhoneNumber = '4324323', @Email = 'jon@gmail.com'
EXEC SelectClient;
-- Probando Transacción de Banco
EXEC InsertBankTransaction @AccountID = 2, @TransactionType = 2, @Amount = 10500
SELECT * FROM BankTransaction
EXEC SelectAccount */
