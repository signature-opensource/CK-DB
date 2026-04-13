--[beginscript]

create table CK.tMCResText
(
	ResId int not null,
	CultureId int not null,
	Value nvarchar(max) not null,
	constraint PK_CK_MCResText primary key (ResId,CultureId),
	constraint FK_CK_MCResText_ResId foreign key (ResId) references CK.tRes(ResId),
	constraint FK_CK_MCResText_CultureId foreign key (CultureId) references CK.tCulture(CultureId)
);

insert into CK.tMCResText( ResId, CultureId, Value ) values( 0, 0, N'' );

insert into CK.tMCResText( ResId, CultureId, Value ) values( 0, 210333265, N'' );
insert into CK.tMCResText( ResId, CultureId, Value ) values( 0, 221277614, N'' );
insert into CK.tMCResText( ResId, CultureId, Value ) values( 1, 210333265, N'Système' );
insert into CK.tMCResText( ResId, CultureId, Value ) values( 1, 221277614, N'System' );

--[endscript]
