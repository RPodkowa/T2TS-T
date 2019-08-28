import { StatusRecord } from './../_model/statusRecord';
import { StatusService } from './../_services/status.service';
import { Component, OnInit } from '@angular/core';
import { Status } from '../_model/status';

@Component({
  selector: 'app-status',
  templateUrl: './status.component.html',
  styleUrls: ['./status.component.less']
})
export class StatusComponent implements OnInit {

  databaseStatus:StatusRecord;
  modulesStatus:StatusRecord;

  constructor(private statusService:StatusService) { }

  ngOnInit() {

    this.statusService.getDatabaseStatus().subscribe(data =>{
      var record:StatusRecord = JSON.parse(data.toString());
      this.databaseStatus = record;
      console.log(this.databaseStatus);
    });

    this.statusService.getModulesStatus().subscribe(data =>{
      var record:StatusRecord = JSON.parse(data.toString());
      this.modulesStatus = record;
      console.log(this.modulesStatus);
    });
  }

}
