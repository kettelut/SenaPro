const PROXY_CONFIG = [
  {
    context: [
      "/senapro",
    ],
    target: "https://localhost:7235/",
    secure: true
  }
]

module.exports = PROXY_CONFIG;
