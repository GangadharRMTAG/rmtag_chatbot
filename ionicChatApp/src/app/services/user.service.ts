import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:8080/api/user'; 

  constructor(private http: HttpClient) {}

  addUser(user: { username: string; roomname: string }): Observable<any> {
    return this.http.post(this.apiUrl, user);
  }
}
