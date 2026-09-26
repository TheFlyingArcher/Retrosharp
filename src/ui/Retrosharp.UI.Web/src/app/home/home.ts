import { Component } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';

interface MockRow {
  name: string;
  age: number;
  days?: number;
  deceased: boolean;
}

const MOCK_DATA: MockRow[] = [
  { name: 'Freddie Freeman', age: 37, deceased: false },
  { name: 'Franquelis Osoria', age: 45, deceased: false },
  { name: 'Anthony Mahoney', age: 31, days: 13, deceased: true }
]

@Component({
  selector: 'app-home',
  imports: [MatTableModule, MatIconModule],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  displayedColumns: string[] = ['name', 'age'];
  dataSource: MockRow[] = MOCK_DATA;

  formatAge(row: MockRow): string {
    if (row.deceased) {
      const days = (row.days ?? 0).toString().padStart(3, '0');
      return `${row.age}y ${days}d`;
    }
    return `${row.age}`;
  }
}
