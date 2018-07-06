import { Http } from '@angular/http';
import { Injectable } from '@angular/core';

@Injectable()
export class AuthService {
  constructor(private http: Http) {
  }
  isAuthenticated: boolean = false;
  access_token: string;
  //ticket: Ticket;
  user: string;

  public login() {
    this.http.get("/.auth/me").subscribe(result => {
      var response = result.json();
      this.access_token = response.access_token;
      var test = response.claims.find(c => c.typ === "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
      this.user = test;
      this.isAuthenticated = true;
    }, error => {
      console.error(error);
      this.user = error;
      this.isAuthenticated = false;
    });

    //var token: LoginToken;
    //token.access_token = "748160993732-5uqm6arq75f2e37bvgnpsjbqj7jfoq14.apps.googleusercontent.com";
    //this.http.post(`${this.baseUrl}/.auth/login/google`, token).subscribe(result => {
    //  console.info(result);
    //},
    //  error => { console.error(error); });
  }
}

interface Ticket {
  access_token: string;
  user_id: string;
  provider_name: string;
  claims: Claim[];
}

class LoginToken {
  access_token: string;
}

interface Claim {
  typ: string;
  val: string;
}
