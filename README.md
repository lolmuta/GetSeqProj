# GetSeqProj
請修改 class Program 
  ConnString 連線字串

使用 sqlserver 資料庫，
  就只需建立二個table 即可運行
  
CREATE TABLE [dbo].[AAA](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SeqNo] [bigint] NOT NULL
) ON [PRIMARY]

CREATE TABLE [dbo].[SeqTable](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SeqId] [nchar](10) NOT NULL,
	[SeqNo] [bigint] NOT NULL,
 CONSTRAINT [PK_SeqTable] PRIMARY KEY CLUSTERED 
(
	[SeqId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

INSERT [dbo].[SeqTable] ([Id], [SeqId], [SeqNo]) VALUES (1, N'Doc1      ', 39)
