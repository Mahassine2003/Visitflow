import {
  animate,
  query,
  stagger,
  state,
  style,
  transition,
  trigger,
  keyframes,
} from '@angular/animations';

export const authFieldsEnter = trigger('authFieldsEnter', [
  transition(':enter', [
    query(
      '.vf-anim-field',
      [
        style({ opacity: 0, transform: 'translateY(14px)' }),
        stagger(
          80,
          [
            animate(
              '420ms cubic-bezier(0.4, 0, 0.2, 1)',
              style({ opacity: 1, transform: 'translateY(0)' })
            ),
          ]
        ),
      ],
      { optional: true }
    ),
  ]),
]);

export const authFormShake = trigger('authFormShake', [
  state('idle', style({ transform: 'translate3d(0,0,0)' })),
  state('shake', style({ transform: 'translate3d(0,0,0)' })),
  transition('idle => shake', [
    animate(
      '520ms cubic-bezier(0.36, 0.07, 0.19, 0.97)',
      keyframes([
        style({ transform: 'translate3d(0,0,0)', offset: 0 }),
        style({ transform: 'translate3d(-10px,0,0)', offset: 0.15 }),
        style({ transform: 'translate3d(10px,0,0)', offset: 0.3 }),
        style({ transform: 'translate3d(-8px,0,0)', offset: 0.45 }),
        style({ transform: 'translate3d(8px,0,0)', offset: 0.6 }),
        style({ transform: 'translate3d(0,0,0)', offset: 1 }),
      ])
    ),
  ]),
  transition('shake => idle', [animate('0ms')]),
]);

export const authSuccessPop = trigger('authSuccessPop', [
  transition(':enter', [
    style({ opacity: 0, transform: 'scale(0.6)' }),
    animate(
      '420ms cubic-bezier(0.34, 1.56, 0.64, 1)',
      style({ opacity: 1, transform: 'scale(1)' })
    ),
  ]),
  transition(':leave', [
    animate('200ms ease', style({ opacity: 0, transform: 'scale(0.9)' })),
  ]),
]);
