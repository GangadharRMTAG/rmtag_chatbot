import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AppConfig } from '../app.config';
import { AuthService } from '../services/auth.service';
import { WebSocketService } from '../services/websocket.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup; 
  fb = inject(FormBuilder); 
  private webSocketService = inject(WebSocketService);

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  onLogin() {
    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.value;
      const credentials = { email, password };

      this.authService.login(credentials).subscribe(
        (response) => {
          alert(response.message);

          const username = response.username;
          const roomname = response.roomname;

          console.log('Establishing WebSocket connection:', { username, roomname });
          this.webSocketService.connect(username, roomname);

          this.router.navigate(['/chat'], {
            queryParams: { username, roomname },
          });
        },
        (error) => {
          alert(error.error.message);
        }
      );
    } else {
      console.log('Form is not valid');
    }
  }

  resetForm() {
    this.loginForm.reset();
  }

  ionViewWillEnter() {
    this.resetForm();
  }
}