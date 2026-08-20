/**
 * Formats a place as "City, StateProvince, Country", dropping the state/province
 * segment when it isn't present (e.g. "Cienfuegos, Cuba"). Returns null when no
 * part of the place is known.
 */
export function formatPlace(
  city: string | null,
  stateProvince: string | null,
  country: string | null,
): string | null {
  const parts = [city, stateProvince, country].filter((part): part is string => !!part);
  return parts.length > 0 ? parts.join(', ') : null;
}

/** Formats a height given in total inches as "[ft]' [in]"". */
export function formatHeight(totalInches: number | null): string | null {
  if (totalInches == null) {
    return null;
  }

  // Round the total first, then split -- rounding feet and the inches
  // remainder independently can roll the remainder up to 12 (e.g. 71.5in
  // becoming "5' 12"" instead of "6' 0"").
  const rounded = Math.round(totalInches);
  const feet = Math.floor(rounded / 12);
  const inches = rounded % 12;
  return `${feet}' ${inches}"`;
}

/** Whole-years age as of `asOf`, given a birth date. Both are ISO 8601 date strings. */
export function formatAge(birthDate: string | null, asOf: Date): number | null {
  if (birthDate == null) {
    return null;
  }

  const birth = new Date(birthDate);
  let age = asOf.getFullYear() - birth.getFullYear();
  const hasHadBirthdayThisYear =
    asOf.getMonth() > birth.getMonth() ||
    (asOf.getMonth() === birth.getMonth() && asOf.getDate() >= birth.getDate());
  if (!hasHadBirthdayThisYear) {
    age--;
  }

  return age;
}
