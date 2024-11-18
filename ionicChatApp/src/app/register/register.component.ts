import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AppConfig } from '../app.config';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent  {
  private baseUrl = AppConfig.baseUrl;
  registerData = {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
  };

  constructor(private http: HttpClient, private router: Router) {}

  register() {
    if (!this.registerData.firstName || !this.registerData.lastName || !this.registerData.email || !this.registerData.password) {
      alert('All fields are required!');
      return;
    }

     this.http.post(`${this.baseUrl}/api/login/register`, this.registerData).subscribe({
      next: (res) => {
        console.log('Registration successful:', res);
        alert('Registration successful!');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        console.error('Registration failed:', err);
        if (err.error?.message === 'Email already registered!') {
          alert('This email is already taken. Please use another email.');
        } else {
          alert(err.error?.message || 'Registration failed. Try again.');
        }
      },
    });
  } 
}