-- SetupConfig: {}
--
-- For each (Resource, ExtendedCulture) the view picks the value stored on the
-- first normalized culture of the fallback chain (CK.tCultureFallback) that
-- holds a row in tMCResString for that resource. The English fallback is implicit
-- because CK.sCultureRegister always appends English to the chain.
--
create view CK.vMCResString
as
	select  r.ResId,
			c.ExtendedCultureId as CultureId,
			v.CultureId as MatchedCultureId,
			v.Value
		from CK.tRes r
		cross join CK.tExtendedCulture c
		outer apply (select top(1) s.CultureId, s.Value
						from CK.tMCResString s
						inner join CK.tCultureFallback m on m.FallbackCultureId = s.CultureId
						where s.ResId = r.ResId and m.CultureId = c.ExtendedCultureId
						order by m.Idx) v
		where c.ExtendedCultureId <> 0;
