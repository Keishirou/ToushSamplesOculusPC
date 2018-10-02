#define SPEED 255

void setup() {
  Serial.begin(9600);
  // モーターAの制御用ピン設定
  pinMode(12, OUTPUT); // 回転方向 (HIGH/LOW)
  pinMode(9, OUTPUT); // ブレーキ (HIGH/LOW)
  pinMode(3, OUTPUT); // A PWMによるスピード制御 (0-255)

  // モーターBの制御用ピン設定
  pinMode(13, OUTPUT); // 回転方向 (HIGH/LOW)
  pinMode(8, OUTPUT); // ブレーキ (HIGH/LOW)
  pinMode(11, OUTPUT); // PWMによるスピード制御 (0-255)
}

void loop(){
  // モーターA: フルスピード正転
  digitalWrite(12, HIGH);
  digitalWrite(9, LOW);
  analogWrite(3, 255);
  // モーターB: フルスピード正転
  digitalWrite(13, HIGH);
  digitalWrite(8, LOW);
  analogWrite(11, 255);
  // 2秒間上記設定で回転
  delay(10000);

  // モーターA: フルスピード逆転
  digitalWrite(12, LOW);
  digitalWrite(9, LOW);
  analogWrite(3, 255);
  // モーターB: フルスピード逆転
  digitalWrite(13, LOW);
  digitalWrite(8, LOW);
  analogWrite(11, 255);
  // 2秒間上記設定で回転
  delay(2000);
}
