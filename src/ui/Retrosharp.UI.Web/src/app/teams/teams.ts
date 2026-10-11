import { Component } from '@angular/core';
import { CurrentFranchises } from './current-franchises/current-franchises';
import { FormerNames } from './former-names/former-names';

@Component({
  selector: 'app-teams',
  templateUrl: './teams.html',
  styleUrl: './teams.css',
  imports: [CurrentFranchises, FormerNames],
})
export class Teams {}
