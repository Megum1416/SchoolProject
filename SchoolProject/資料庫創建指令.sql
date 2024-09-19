create database SchoolProject

use SchoolProject

create table Members(
	MemberID char(8) not null ,
	Email varchar(255) not null, --登入帳號
	Password varchar(30) not null,	--登入密碼
	UserName nvarchar(30) not null, --顯示出來的暱稱
	LodestoneID char(8) not null ,
	FirstName char(15) not null , --爬
	FamilyName char(15) not null , --爬
	DataCenter varchar(30) not null, --爬
	ServerName varchar(30) not null, --爬
	CreatedAt datetime not null,
	UpdatedAt datetime,
	Photos varbinary(MAX),
	ImageType nvarchar(MAX),
	IsAdmin bit,
	primary key(MemberID)
)

create table FollowList(
	FollowID char(8) not null ,
	MemberID char(8) not null , --追隨者
	LodestoneID char(8) not null , --被追隨者
	CreatedAt datetime not null,
	UpdatedAt datetime,
	primary key(FollowID)
)

create table Post(
	PostID char(8) not null ,
	MemberID char(8) not null , --哪個會員發的文
	PostTitle nvarchar(30) not null , --文章標題
	Description nvarchar(250) not null , --文章內容
	CreatedAt datetime not null,
	UpdatedAt datetime,
	Photos varbinary(MAX),
	ImageType nvarchar(MAX),
	primary key(PostID)
)

create table RePost(
	RePostID char(8) not null ,
	PostID char(8) not null , --回復哪則文章
	Description nvarchar(250) not null , --回覆內容
	CreatedAt datetime not null,
	primary key(RePostID)
)



alter table [FollowList]
	add foreign key(MemberID) references Members(MemberID)

alter table [Post] --哪個會員發的留言
	add foreign key(MemberID) references Members(MemberID)

alter table [RePost] --回覆給哪一則文章的
	add foreign key(PostID) references Post(PostID)