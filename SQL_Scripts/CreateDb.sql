USE [master]
GO

DECLARE @DbName varchar(60) = 'KeplerNew'
DECLARE @COMMAND_TEMPLATE VARCHAR(MAX)
DECLARE @SQL_SCRIPT VARCHAR(MAX)
DECLARE @USE_DB_TEMPLATE varchar(60) 
DECLARE @USE_DB_SCRIPT VARCHAR(60)

set @DbName = QUOTENAME(@DbName)

EXEC('CREATE DATABASE '+ @DbName)

SET @COMMAND_TEMPLATE='ALTER DATABASE {DBNAME} SET  READ_WRITE '
SET @SQL_SCRIPT = REPLACE(@COMMAND_TEMPLATE, '{DBNAME}', @DbName)
EXECUTE (@SQL_SCRIPT)


/****** Object:  Table [dbo].[BaseLines]    Script Date: 5/31/2016 8:42:14 PM ******/

SET @COMMAND_TEMPLATE='USE {DBNAME};
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE TABLE {DBNAME}.[dbo].[BaseLines](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BranchId] [bigint] NOT NULL,
	[Name] [nvarchar](700) NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_dbo.BaseLines] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]'

SET @SQL_SCRIPT = REPLACE(@COMMAND_TEMPLATE, '{DBNAME}', @DbName)
EXECUTE (@SQL_SCRIPT)

GO




