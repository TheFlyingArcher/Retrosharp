import { formatGamesOverFiveHundred } from './format.util';

describe('formatGamesOverFiveHundred', () => {
  it('prefixes a plus sign for a winning record', () => {
    expect(formatGamesOverFiveHundred(90, 72)).toBe('+18');
  });

  it('uses a minus sign for a losing record', () => {
    expect(formatGamesOverFiveHundred(60, 102)).toBe('-42');
  });

  it('returns 0 for exactly .500', () => {
    expect(formatGamesOverFiveHundred(81, 81)).toBe('0');
  });
});
