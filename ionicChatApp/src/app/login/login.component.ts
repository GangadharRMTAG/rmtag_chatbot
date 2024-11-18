import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AppConfig } from '../app.config';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  private baseUrl = AppConfig.baseUrl;

  ngOnInit() {}
  loginData = {
    email: '',
    password: '',
  };

  constructor(private http: HttpClient, private router: Router) {}

  login() {
    if (!this.loginData.email || !this.loginData.password) {
      alert('Email and Password are required!');
      return;
    }

     this.http.post(`${this.baseUrl}/api/login/login`, this.loginData).subscribe({
      next: (res) => {
        console.log('Login successful:', res);
        alert('Login successful!');
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Login failed:', err);
        alert(err.error?.message || 'Invalid email or password.');
      },
    });
  }
}