import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable()
export class AuthService {
  constructor(private http: HttpClient) {
    this.http.get<Ticket[]>("https://eventregistratorweb.azurewebsites.net/.auth/me").subscribe(ticket => {
      try {
        var firstName = ticket[0].user_claims.find(c => c.typ === "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").val;
        var lastName = ticket[0].user_claims.find(c => c.typ === "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname").val;
        this.user = firstName;
        this.isAuthenticated = true;
      } catch (ex) {
        console.log(ex);
      }
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
  isAuthenticated: boolean = false;
  user: string;
}

class Ticket {
  access_token: string;
  id_token: string;
  user_id: string;
  provider_name: string;
  user_claims: Claim[];
}

class Claim {
  typ: string;
  val: string;
}
