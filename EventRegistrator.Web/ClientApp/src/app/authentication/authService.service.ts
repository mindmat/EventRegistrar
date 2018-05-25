import { Inject, } from '@angular/core';
import { Http } from '@angular/http';
import { Injectable } from '@angular/core';

@Injectable()
export class AuthService {
  constructor(private http: Http , @Inject('BASE_URL') private baseUrl: string) {
    http.get(`${baseUrl}/.auth/me`).subscribe(result => {
      var response = result.json();
      this.access_token = response.access_token;
      var test = response.claims.find(c => c.typ === "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
    }, error => console.error(error));
  }
  access_token: string;
  //ticket: Ticket;
}

interface Ticket {
  access_token: string;
  user_id: string;
  provider_name: string;
  claims: Claim[];
}

interface Claim {
  typ: string;
  val: string;
}
