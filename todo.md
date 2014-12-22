
Session

- [ ] Allow old passphrase so that passphrases can be rotated
- [ ] Specify which paths in the app will use the cookie
- [ ] Specify the path for the cookie within the browser  e.g /api/xyz
- [ ] Dont set-cookie if the inbound and outbound values match

EventSource

- [x] Add a class to format/parse HTML5 EventSource format
- [x] Some unit tests!!


Routes

- [ ] Use a regexp with named captures in place of all the Tokenising logic
- [ ] Generalise public Route function to take array of templates and an array of httpMethod
- [ ] RouteHeader should also handle outboud headers using OnSendingHeaders to extract from the teouteParam

Cookies

- [ ] Set appropriate domain and path when returning a cookie. Maybe use the
      PathBase as a good default?
