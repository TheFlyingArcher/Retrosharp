import { inject, Service } from '@angular/core';
import { HttpService } from './http.service';

@Service()
export class PlayerService {
  private http:HttpService = inject(HttpService)
}
