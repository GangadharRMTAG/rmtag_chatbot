import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { WebSocketService } from '../services/websocket.service'; 
import { AppConfig } from '../app.config';

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
  joinRoomForm!: FormGroup;
  fb = inject(FormBuilder);
  private websocketService: WebSocketService = inject(WebSocketService);

  private baseUrl = AppConfig.baseUrl;

  constructor(private route: Router, private http: HttpClient) { }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.joinRoomForm = this.fb.group({
      user: ['', Validators.required],
      room: ['', Validators.required]
    });
  }

  joinRoom() {
    if (this.joinRoomForm.valid) {
      const { user, room } = this.joinRoomForm.value;
      console.log('Joining room with', { user, room });

     this.http.post(`${this.baseUrl}/api/user/join`, {Username: user, roomname: room})
        .subscribe(response => {
          console.log('User added:', response);
          this.route.navigate(['/chat',{ roomname: room }]);
          
          this.websocketService.connect(user, room);
        }, error => {
          console.error('Error adding user:', error);
        });
    } else {
      console.log('Form is not valid');
    }
  }

  resetForm() {
    this.joinRoomForm.reset();
  }

  ionViewWillEnter() {
    this.resetForm();
  }
  
}
