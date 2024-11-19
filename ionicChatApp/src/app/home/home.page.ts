import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { WebSocketService } from '../services/websocket.service'; 
import { AppConfig } from '../app.config';
import { AuthService } from '../services/auth.service';

interface JoinRoomResponse {
  message: string;
  navigate: boolean;
}
@Component({
  selector: 'app-home',
  templateUrl: './home.page.html',
  styleUrls: ['./home.page.scss'],
})
export class HomePage implements OnInit {
  username = '';
  roomname = '';
  email = '';
  password = '';

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {}

  onRegister() {
    const user = {
      username: this.username,
      roomname: this.roomname,
      email: this.email,
      password: this.password
    };
  
    this.authService.register(user).subscribe(
      (response) => {
        alert(response.message);
        this.router.navigate(['/login']);
      },
      (error) => {
        alert(error.error.message);
      }
    );
  }
}