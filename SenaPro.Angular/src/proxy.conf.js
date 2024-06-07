const PROXY_CONFIG = [
  {
    context: [
      "/cbd",
    ],
    target: "https://localhost:7235/",
    secure: true
  }
]

module.exports = PROXY_CONFIG;
