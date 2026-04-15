import {
  animate,
  group,
  query,
  style,
  transition,
  trigger,
} from '@angular/animations';

const EASE = 'cubic-bezier(0.4, 0, 0.2, 1)';

/** Transitions between /login and /register inside auth shell (slide + fade). */
export const authShellRouteAnimation = trigger('authShellRoute', [
  transition('authLogin => authRegister', [
    style({ position: 'relative' }),
    query(
      ':enter, :leave',
      [
        style({
          position: 'absolute',
          left: 0,
          right: 0,
          top: 0,
          width: '100%',
          minHeight: '100%',
        }),
      ],
      { optional: true }
    ),
    query(
      ':leave',
      [
        style({
          opacity: 1,
          transform: 'translate3d(0,0,0)',
        }),
      ],
      { optional: true }
    ),
    query(
      ':enter',
      [
        style({
          opacity: 0,
          transform: 'translate3d(24px,0,0)',
        }),
      ],
      { optional: true }
    ),
    group([
      query(
        ':leave',
        [
          animate(
            `320ms ${EASE}`,
            style({
              opacity: 0,
              transform: 'translate3d(-20px,0,0)',
            })
          ),
        ],
        { optional: true }
      ),
      query(
        ':enter',
        [
          animate(
            `420ms 40ms ${EASE}`,
            style({
              opacity: 1,
              transform: 'translate3d(0,0,0)',
            })
          ),
        ],
        { optional: true }
      ),
    ]),
  ]),
  transition('authRegister => authLogin', [
    style({ position: 'relative' }),
    query(
      ':enter, :leave',
      [
        style({
          position: 'absolute',
          left: 0,
          right: 0,
          top: 0,
          width: '100%',
          minHeight: '100%',
        }),
      ],
      { optional: true }
    ),
    query(
      ':leave',
      [style({ opacity: 1, transform: 'translate3d(0,0,0)' })],
      { optional: true }
    ),
    query(
      ':enter',
      [
        style({
          opacity: 0,
          transform: 'translate3d(-24px,0,0)',
        }),
      ],
      { optional: true }
    ),
    group([
      query(
        ':leave',
        [
          animate(
            `320ms ${EASE}`,
            style({
              opacity: 0,
              transform: 'translate3d(20px,0,0)',
            })
          ),
        ],
        { optional: true }
      ),
      query(
        ':enter',
        [
          animate(
            `420ms 40ms ${EASE}`,
            style({
              opacity: 1,
              transform: 'translate3d(0,0,0)',
            })
          ),
        ],
        { optional: true }
      ),
    ]),
  ]),
]);
