CREATE DATABASE [CinemaDB];
GO 

USE [CinemaDB];
GO

CREATE TABLE [users] (
	[id] int IDENTITY(1,1) NOT NULL,
	[email] nvarchar(255) NOT NULL,
	[password_hash] nvarchar(255) NOT NULL,
	[role] nvarchar(50) NOT NULL,
	[full_name] nvarchar(255) NOT NULL,
	PRIMARY KEY ([id])
);

CREATE TABLE [movies] (
	[id] int IDENTITY(1,1) NOT NULL,
	[title] nvarchar(255) NOT NULL,
	[description] nvarchar(max) NOT NULL,
	[director] nvarchar(255) NOT NULL,
	[cast] nvarchar(max) NOT NULL,
	[duration] int NOT NULL,
	[release_date] datetime NOT NULL,
	[poster_url] nvarchar(255) NOT NULL,
	PRIMARY KEY ([id])
);

CREATE TABLE [genres] (
	[id] int IDENTITY(1,1) NOT NULL,
	[name] nvarchar(100) NOT NULL,
	PRIMARY KEY ([id])
);

CREATE TABLE [halls] (
	[id] int IDENTITY(1,1) NOT NULL,
	[name] nvarchar(100) NOT NULL,
	[capacity] int NOT NULL,
	PRIMARY KEY ([id])
);

CREATE TABLE [moviegenres] (
	[movie_id] int NOT NULL,
	[genre_id] int NOT NULL
);

CREATE TABLE [sessions] (
	[id] int IDENTITY(1,1) NOT NULL,
	[movie_id] int NOT NULL,
	[hall_id] int NOT NULL,
	[start_time] datetime NOT NULL,
	[price] decimal(10,2) NOT NULL,
	PRIMARY KEY ([id])
);

CREATE TABLE [tickets] (
	[id] int IDENTITY(1,1) NOT NULL,
	[user_id] int NOT NULL,
	[session_id] int NOT NULL,
	[seat_number] int NOT NULL,
	[purchase_date] datetime NOT NULL,
	PRIMARY KEY ([id])
);

ALTER TABLE [moviegenres] ADD CONSTRAINT [moviegenres_fk0] FOREIGN KEY ([movie_id]) REFERENCES [movies]([id]);
ALTER TABLE [moviegenres] ADD CONSTRAINT [moviegenres_fk1] FOREIGN KEY ([genre_id]) REFERENCES [genres]([id]);

ALTER TABLE [sessions] ADD CONSTRAINT [sessions_fk1] FOREIGN KEY ([movie_id]) REFERENCES [movies]([id]);
ALTER TABLE [sessions] ADD CONSTRAINT [sessions_fk2] FOREIGN KEY ([hall_id]) REFERENCES [halls]([id]);

ALTER TABLE [tickets] ADD CONSTRAINT [tickets_fk1] FOREIGN KEY ([user_id]) REFERENCES [users]([id]);
ALTER TABLE [tickets] ADD CONSTRAINT [tickets_fk2] FOREIGN KEY ([session_id]) REFERENCES [sessions]([id]);
