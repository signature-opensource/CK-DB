-- SetupConfig: {}
--
-- For each (Resource, Culture) the view resolves the value in three steps:
--   1. Walk the ParentCultureId chain from the requested culture and pick
--      the closest stored value.
--   2. If nothing is found, fall back to the English culture
--      (CultureId 221277614), even when it is not part of the requested
--      culture's parent chain.
--   3. If nothing is still found but the resource exists in some other
--      language, return that value (smallest CultureId wins, for
--      deterministic results).
--
create view CK.vMCResText
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
			coalesce( v.MatchedCultureId, en.MatchedCultureId, any_lang.MatchedCultureId ) as MatchedCultureId,
			coalesce( v.Value, en.Value, any_lang.Value ) as Value
		from CK.tRes r
		cross join (select CultureId from CK.tCulture where CultureId <> 0) root
		outer apply (select top(1) s.CultureId as MatchedCultureId, s.Value
						from CK.tMCResText s
						inner join Chain ch on ch.CultureId = s.CultureId
						where ch.RootCultureId = root.CultureId and s.ResId = r.ResId
						order by ch.Depth) v
		outer apply (select s.CultureId as MatchedCultureId, s.Value
						from CK.tMCResText s
						where s.CultureId = 221277614 and s.ResId = r.ResId) en
		outer apply (select top(1) s.CultureId as MatchedCultureId, s.Value
						from CK.tMCResText s
						where s.ResId = r.ResId
						order by s.CultureId) any_lang;
