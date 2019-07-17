import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { StatusRecord } from '../_model/statusRecord';

@Injectable({
  providedIn: 'root'
})
export class StatusService {

  constructor(private http:HttpClient) { }

  public getStatus(){
  return this.http.get("http://www.thea2pl.webd.pro/thea2pl.webd.pro/translator/www/status.txt"
    ,{ responseType: 'text' as 'json'});
  }
}
