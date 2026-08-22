import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';
import { BattingLine } from '../../model/batting-line.model';
import { PitchingLine } from '../../model/pitching-line.model';
import { PlayerDetail as PlayerDetailModel } from '../../model/player-detail.model';
import { PlayerService } from '../../service/player.service';
import { PlayerDetail } from './player-detail';

const PLAYER: PlayerDetailModel = {
  id: 1,
  retroSheetId: 'ruthb101',
  surname: 'Ruth',
  useName: 'Babe Ruth',
  fullName: 'George Herman Ruth',
  birthDate: '1895-02-06T00:00:00',
  birthCity: 'Baltimore',
  birthStateProvince: 'Maryland',
  birthCountry: 'USA',
  deathDate: '1948-08-16T00:00:00',
  deathCity: 'New York',
  deathStateProvince: 'New York',
  deathCountry: 'USA',
  cemetery: 'Gate of Heaven Cemetery',
  cemeteryCity: 'Hawthorne',
  cemeteryStateProv: 'New York',
  cemeteryCountry: 'USA',
  cemeteryNote: null,
  bats: 'L',
  throws: 'L',
  height: 74,
  weight: 215,
  isHof: true,
  playerDebutDate: '1914-07-11T00:00:00',
  playerLastDate: '1935-05-30T00:00:00',
  managerDebutDate: null,
  managerLastDate: null,
  coachDebutDate: null,
  coachLastDate: null,
  umpireDebutDate: null,
  umpireLastDate: null,
};

function statsResponse<T>(rows: T[]) {
  return { rows, combinedTotal: null };
}

const BATTING_LINE: BattingLine = {
  franchiseCode: 'NYA',
  franchiseName: 'New York Yankees',
  seasonYear: 1920,
  plateAppearances: 616,
  atBats: 458,
  hits: 172,
  doubles: 36,
  triples: 9,
  homeruns: 54,
  baseOnBalls: 150,
  strikeouts: 80,
  sacrificeFlies: 0,
  sacrificeBunts: 6,
  intentionalBb: 0,
  hitByPitches: 3,
  stolenBases: 14,
  timesCaughtStealing: 0,
  runs: 158,
  groundedIntoDoublePlay: 0,
  runsBattedIn: 137,
  gamesPlayed: 142,
  gamesStarted: 142,
  totalBases: 388,
  battingAverage: 0.376,
  onBasePercentage: 0.532,
  sluggingPercentage: 0.847,
  onBasePlusSlugging: 1.379,
  battingAverageOnBallsInPlay: 0.361,
  isolatedPower: 0.471,
};

const PITCHING_LINE: PitchingLine = {
  franchiseCode: 'NYA',
  franchiseName: 'New York Yankees',
  seasonYear: 1921,
  gamesPitched: 1,
  gamesStarted: 0,
  gamesFinished: 0,
  completeGames: 0,
  shutouts: 0,
  saves: 0,
  inningsPitchedDisplay: '1.0',
  hits: 1,
  runs: 0,
  earnedRuns: 0,
  baseOnBalls: 0,
  strikeouts: 1,
  intentionalBb: 0,
  hitBatsmen: 0,
  balks: 0,
  wildPitches: 0,
  era: 0,
  whip: 1,
  strikeoutsPerNine: 9,
  walksPerNine: 0,
  homeRunsPerNine: 0,
  homeRunsPerFlyBall: 0,
  fip: 0,
  battingAverageOnBallsInPlay: 0,
  fipConstant: null,
  fipConstantLeagueCode: null,
  fipConstantSeasonYear: null,
};

function createServiceMock() {
  return {
    browseAsync: vi.fn(),
    getByIdAsync: vi.fn().mockReturnValue(of(PLAYER)),
    getBattingAsync: vi.fn().mockReturnValue(of(statsResponse<BattingLine>([]))),
    getPitchingAsync: vi.fn().mockReturnValue(of(statsResponse<PitchingLine>([]))),
  };
}

describe('PlayerDetail', () => {
  let fixture: ComponentFixture<PlayerDetail>;
  let component: PlayerDetail;
  let service: ReturnType<typeof createServiceMock>;

  beforeEach(async () => {
    service = createServiceMock();

    await TestBed.configureTestingModule({
      imports: [PlayerDetail],
      providers: [{ provide: PlayerService, useValue: service as unknown as PlayerService }],
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerDetail);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('id', '1');
  });

  it('should create and load the player', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(component.displayName()).toBe('Babe Ruth');
  });

  it('formats height, birthplace, and death place', () => {
    fixture.detectChanges();
    expect(component.height()).toBe(`6' 2"`);
    expect(component.birthPlace()).toBe('Baltimore, Maryland, USA');
    expect(component.deathPlace()).toBe('New York, New York, USA');
    expect(component.burialLocation()).toBe('Gate of Heaven Cemetery, Hawthorne, New York, USA');
  });

  it('hides the batting and pitching tables when there are no rows', () => {
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('app-statistics-table'))).toBeNull();
  });

  it('shows the batting table when batting rows are present, but not pitching', () => {
    service.getBattingAsync.mockReturnValue(of(statsResponse<BattingLine>([BATTING_LINE])));
    fixture.detectChanges();

    const tables = fixture.debugElement.queryAll(By.css('app-statistics-table'));
    expect(tables.length).toBe(1);
  });

  it('shows both tables for a player with both batting and pitching rows', () => {
    service.getBattingAsync.mockReturnValue(of(statsResponse<BattingLine>([BATTING_LINE])));
    service.getPitchingAsync.mockReturnValue(of(statsResponse<PitchingLine>([PITCHING_LINE])));
    fixture.detectChanges();

    const tables = fixture.debugElement.queryAll(By.css('app-statistics-table'));
    expect(tables.length).toBe(2);
  });

  it('sets an error message and stops loading when the load fails', () => {
    service.getByIdAsync.mockReturnValue(throwError(() => new Error('boom')));
    vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();

    expect(component.error()).toBe('Unable to load player. Please try again later.');
    expect(component.loading()).toBe(false);
  });

  it('shows a "not found" message for a 404', () => {
    service.getByIdAsync.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 404 })),
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();

    expect(component.error()).toBe('Player not found.');
  });
});
