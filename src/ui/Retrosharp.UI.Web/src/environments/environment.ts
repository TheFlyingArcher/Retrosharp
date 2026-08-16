export const environment = {
  /**
   * Relative so it resolves against whatever origin is serving the app -- the dev server proxies
   * it to Retrosharp.UI.Api (see proxy.conf.json), and a production deployment is expected to
   * reverse-proxy the same path to the API.
   */
  apiBaseUrl: '/api',
};
