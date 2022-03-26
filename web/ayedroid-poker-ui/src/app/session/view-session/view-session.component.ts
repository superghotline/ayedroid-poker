import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'pkr-view-session',
  templateUrl: './view-session.component.html',
  styleUrls: ['./view-session.component.scss'],
})
export class ViewSessionComponent implements OnInit {
  public sessionId: string = '';

  public constructor(private activatedRoute: ActivatedRoute) {}

  public ngOnInit(): void {
    this.sessionId = this.activatedRoute.snapshot.params['sessionId'];
  }
}