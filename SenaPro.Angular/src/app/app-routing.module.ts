import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SenaProComponent } from './calculadora-cdb/calculadora-cdb.component';

const routes: Routes = [
  { path: '', redirectTo: 'calculadora-cdb', pathMatch: 'full' },
  { path: 'calculadora-cdb', component: SenaProComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
