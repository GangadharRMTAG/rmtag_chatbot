import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule,ReactiveFormsModule } from '@angular/forms';
import { HomePage } from './home.page';
import { HttpClient } from '@angular/common/http';
import { HomePageRoutingModule } from './home-routing.module';
import { HttpClientModule } from '@angular/common/http'; 


@NgModule({
  imports: [
    CommonModule,
    FormsModule,ReactiveFormsModule,
    IonicModule,
    HomePageRoutingModule,HttpClientModule
  ],
  declarations: [HomePage]
})
export class HomePageModule {}