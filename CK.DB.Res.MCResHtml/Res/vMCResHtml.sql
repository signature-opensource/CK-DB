-- SetupConfig: {}
create view CK.vMCResHtml
as
	with Chain as
	(
		select CultureId as RootCultureId, CultureId, ParentCultureId, 0 as Depth
			from CK.tCulture
			where CultureId <> 0
		union all
		select ch.RootCultureId, p.CultureId, p.ParentCultureId, ch.Depth + 1
			from Chain ch
			inner join CK.tCulture p on p.CultureId = ch.ParentCultureId
	)
	select  r.ResId,
			root.CultureId,
			v.MatchedCultureId,
			v.Value
		from CK.tRes r
		cross join (select CultureId from CK.tCulture where CultureId <> 0) root
		outer apply (select top(1) s.CultureId as MatchedCultureId, s.Value
						from CK.tMCResHtml s
						inner join Chain ch on ch.CultureId = s.CultureId
						where ch.RootCultureId = root.CultureId and s.ResId = r.ResId
						order by ch.Depth) v;
