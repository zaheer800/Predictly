import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth';

@Component({
  selector: 'app-nav',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
  styles: ``,
})
export class Nav {
  constructor(public auth: AuthService) {}

  logout() {
    this.auth.logout();
  }
}
