USE [master]
GO

CREATE DATABASE [IOT]

GO

CREATE TABLE [dbo].[Device](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](90) NOT NULL,
	[publicId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Device] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DeviceAction]    Script Date: 6/25/2018 5:27:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DeviceAction](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[deviceId] [int] NOT NULL,
	[name] [varchar](90) NOT NULL,
	[value] [varchar](4000) NOT NULL,
 CONSTRAINT [PK_Action] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Index [IX_Device]    Script Date: 6/25/2018 5:27:36 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Device] ON [dbo].[Device]
(
	[publicId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_DeviceAction_DeviceId]    Script Date: 6/25/2018 5:27:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_DeviceAction_DeviceId] ON [dbo].[DeviceAction]
(
	[deviceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DeviceAction]  WITH CHECK ADD  CONSTRAINT [FK_DeviceAction_Device] FOREIGN KEY([deviceId])
REFERENCES [dbo].[Device] ([id])
GO
ALTER TABLE [dbo].[DeviceAction] CHECK CONSTRAINT [FK_DeviceAction_Device]
GO
USE [master]
GO
ALTER DATABASE [IOT] SET  READ_WRITE 
GO