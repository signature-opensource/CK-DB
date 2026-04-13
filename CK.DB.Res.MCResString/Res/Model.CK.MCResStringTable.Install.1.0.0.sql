--[beginscript]

create table CK.tMCResString
(
	ResId int not null,
	CultureId int not null,
	Value nvarchar(400) not null,
	constraint PK_CK_MCResString primary key (ResId,CultureId),
	constraint FK_CK_MCResString_ResId foreign key (ResId) references CK.tRes(ResId),
	constraint FK_CK_MCResString_CultureId foreign key (CultureId) references CK.tCulture(CultureId)
);

insert into CK.tMCResString( ResId, CultureId, Value ) values( 0, 0, N'' );

insert into CK.tMCResString( ResId, CultureId, Value ) values( 0, 210333265, N'' );
insert into CK.tMCResString( ResId, CultureId, Value ) values( 0, 221277614, N'' );
insert into CK.tMCResString( ResId, CultureId, Value ) values( 1, 210333265, N'Système' );
insert into CK.tMCResString( ResId, CultureId, Value ) values( 1, 221277614, N'System' );

--[endscript]
