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

  databaseStatus: StatusRecord;
  modulesStatus: StatusRecord;

  modifiedDateForDatabase = "";
  modifiedDateForModules = "";

  dateForDisplay = "";

  constructor(private statusService: StatusService) { }

  ngOnInit() {
    this.statusService.getDatabaseStatus().subscribe(data => {
      var record: StatusRecord = JSON.parse(data.toString());
      this.databaseStatus = record;
     
      this.modifiedDateForDatabase = record.modifiedDate;
      this.onStatusChange();
    });

    this.statusService.getModulesStatus().subscribe(data => {
      var record: StatusRecord = JSON.parse(data.toString());
      this.modulesStatus = record;

      this.modifiedDateForModules = record.modifiedDate;
      this.onStatusChange();    
    });
  }

  onStatusChange() {
    if (this.modifiedDateForDatabase == "") {
      this.dateForDisplay = this.modifiedDateForModules;
    }
    else if (this.modifiedDateForModules == "") {
      this.dateForDisplay = this.modifiedDateForDatabase;
    }
    else {
      var date1 = new Date(this.modifiedDateForDatabase);
      var date2 = new Date(this.modifiedDateForModules);

      if(date1 > date2){
        this.dateForDisplay = this.modifiedDateForDatabase;
      }
      else{
        this.dateForDisplay = this.modifiedDateForModules;
      }
    }
  }

}
