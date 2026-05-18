-- SetupConfig: {}
--
-- For each (Resource, Culture) the view first walks the ParentCultureId
-- chain to locate a stored value. If the chain yields nothing, the English
-- culture (CultureId 221277614) is used as the ultimate fallback, even when
-- it is not part of the requested culture's parent chain.
--
create view CK.vMCResString
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
			coalesce( v.MatchedCultureId, en.MatchedCultureId ) as MatchedCultureId,
			coalesce( v.Value, en.Value ) as Value
		from CK.tRes r
		cross join (select CultureId from CK.tCulture where CultureId <> 0) root
		outer apply (select top(1) s.CultureId as MatchedCultureId, s.Value
						from CK.tMCResString s
						inner join Chain ch on ch.CultureId = s.CultureId
						where ch.RootCultureId = root.CultureId and s.ResId = r.ResId
						order by ch.Depth) v
		outer apply (select s.CultureId as MatchedCultureId, s.Value
						from CK.tMCResString s
						where s.CultureId = 221277614 and s.ResId = r.ResId) en;
