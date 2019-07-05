import { SupportComponent } from './support/support.component';
import { AboutComponent } from './about/about.component';
import { HomeComponent } from './home/home.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { TranslateProgressComponent } from './translate-progress/translate-progress.component';

const routes: Routes = [
  {path:"home", component: HomeComponent},
  {path:"translate-progress", component: TranslateProgressComponent},
  {path:"about", component: AboutComponent},
  {path:"support", component: SupportComponent},
  {path:"**", redirectTo: "/home"}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
