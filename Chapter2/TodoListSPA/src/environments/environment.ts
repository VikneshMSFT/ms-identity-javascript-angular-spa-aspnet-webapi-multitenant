// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  envName : 'dev',
  mainRedirectUri: 'https://localhost:4200',
  redirectUri: 'https://localhost:4200/auth',
  todoListApiResourceUri: 'https://localhost:44351/api/todolist',
  getConfigApiResourceUri: 'https://localhost:44351/api/configurations',
  authApiResourceUri: 'https://localhost:44351/api/auth',
  todoListResourceScope: ['api://6d8d39e8-ccb1-498d-99a0-955a57bd344d/.default'],
  uiClientId: 'fc9c3f24-62b3-4c2c-8eca-39bd17ab5b05'  
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
