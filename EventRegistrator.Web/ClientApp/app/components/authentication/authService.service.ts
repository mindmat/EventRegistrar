import { Component, Inject, } from '@angular/core';
import { Http } from '@angular/http';

export class AuthService {
    constructor(private http: Http, @Inject('BASE_URL') private baseUrl: string) {
        http.get(`${baseUrl}/.auth/me`).subscribe(result => {
            var response = result.json();
            this.accessToken = response.access_token;
            this.userId = response.user_id;
            this.providerName = response.provider_name;
            var claims = response.claims as Claim[];
            var test = claims.find(c => c.typ === "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
        }, error => console.error(error));
    }

    accessToken: string;
    userId: string;
    providerName: any;
}

interface Claim {
    typ: string;
    val: string;
}