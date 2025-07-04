# 패턴 만드는 방법
## 예시
```yaml
description: 2025 10단 곡
options:
  bpm: 150
  minimumNotes: 500
patterns:
# 4-6
- <1>d.<4>d...<3>kkkdddkkddddkkd<6>ddddddkkkkkk<3>dddddd
# 26-35
- "d...d...<6>kkk<4>dddddd
  d.k.k.<6>kkk<4>ddddkkkk d.k.k.<6>kkk<4>ddddkkdk <4>d.k.k.<6>kkk<4>ddddkkdd <4>d.<6>kkk<4>d.dd<6>kkk<4>ddkkdd
  d.kkddkkddddkkkk d.kkddkkddddkkdk d.kkddkk<6>dddddd<4>kkdd d.k.k.ddkkddkkdd
  <1>d.<4>kk<6>ddd<1>k"
```

```yaml
patterns:
- <6>{d..ddd?..d.., ddd?..d..d.., d..d..ddd?..}*4
```

- YAML 포맷으로 되어있어요.
	- `#` 으로 주석을 표기할 수 있어요. 곡 마디 같은거 적어둘 때 유용할 것 같네요.
- description에는 간단한 설명을 적어둘 수 있어요. 패턴을 선택하면 해당 설명이 나와요.
- options에는 기본 설정을 덮어쓸 값들을 지정할 수 있어요.
- patterns에는 연습할 패턴들을 적으세요.
	- 한 패턴은 리스트의 문자열이고 YAML 에서 리스트 아이템은 `- ` 로 시작합니다. 공백이 필요함에 주의하세요.

### 패턴 문법
- d는 동, k는 캇을 의미합니다.
- .는 쉼표입니다.
- ?로 하면 50% 확률로 동, 캇이 정해집니다.
- 공백은 무시됩니다.
- `<n>` 은 박자를 지정합니다. n (1부터 16까지 가능) 은 한 박에 들어갈 노트의 수입니다.
	- 예를 들어 <4>는 16비트, <6>는 24비트
	- 안 적으면 기본적으로 <4> 입니다.
- `{A, B, C} * n` 형식은 A, B, C 중에서 랜덤하게 n번 뽑아서 붙입니다.
	- `{}` 괄호는 매번 랜덤을 뽑고, `()` 괄호는 그 중 하나를 뽑고 n번 반복시킵니다.
- YAML 특성상 맨 처음 글자가 괄호면 인식을 못합니다. 이 경우 <4>를 앞에 붙여주세요.
- 여러줄에 걸쳐서 적고 싶을 때 `"`  로 감싸서 문자열인걸 표시하고 들여쓰기로 인덴트를 맞춰줘야 해요.
