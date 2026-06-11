#!/usr/bin/env node
// Moongate Next Item Template Converter
// Version: 0.1.0-develop.5

"use strict";

const { run } = require("../src/cli");

run(process.argv.slice(2)).catch((error) => {
  console.error(error.message);
  process.exitCode = 1;
});
