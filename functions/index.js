const functions = require("firebase-functions");
const admin = require("firebase-admin");
const nodemailer = require("nodemailer");

admin.initializeApp();

const gmailUser = functions.config().gmail.user;
const gmailPass = functions.config().gmail.pass;

const transporter = nodemailer.createTransport({
  service: "gmail",
  auth: { user: gmailUser, pass: gmailPass },
});

function isValidEmail(email) {
  return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email);
}

exports.sendParentOtp = functions
  .region("asia-southeast1")
  .https.onRequest(async (req, res) => {
    ...
  });
