import { Fragment } from 'react';
import { Dialog, Transition } from '@headlessui/react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import api from '@/services/api';

const InviteSchema = Yup.object().shape({
  email: Yup.string().email('Invalid email').required('Required'),
  role: Yup.string().required('Required'),
});

type Props = {
  open: boolean;
  onClose: () => void;
};

export default function InviteUserModal({ open, onClose }: Props) {
  const queryClient = useQueryClient();

  const { mutate, isLoading } = useMutation({
    mutationFn: (values: { email: string; role: string }) =>
      api.post('/users/invite', values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      onClose();
    },
  });

  return (
    <Transition.Root show={open} as={Fragment}>
      <Dialog as="div" className="relative z-10" onClose={onClose}>
        <Transition.Child
          as={Fragment}
          enter="ease-out duration-300"
          enterFrom="opacity-0"
          enterTo="opacity-100"
          leave="ease-in duration-200"
          leaveFrom="opacity-100"
          leaveTo="opacity-0"
        >
          <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" />
        </Transition.Child>

        <div className="fixed inset-0 z-10 overflow-y-auto">
          <div className="flex min-h-full justify-center p-4 text-center items-center sm:p-0">
            <Transition.Child
              as={Fragment}
              enter="ease-out duration-300"
              enterFrom="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
              enterTo="opacity-100 translate-y-0 sm:scale-100"
              leave="ease-in duration-200"
              leaveFrom="opacity-100 translate-y-0 sm:scale-100"
              leaveTo="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
            >
              <Dialog.Panel className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
                <div>
                  <div className="text-center">
                    <Dialog.Title as="h3" className="text-lg font-semibold leading-6 text-gray-900">
                      Invite User
                    </Dialog.Title>
                  </div>

                  <Formik
                    initialValues={{ email: '', role: 'user' }}
                    validationSchema={InviteSchema}
                    onSubmit={mutate}
                  >
                    {({ errors, touched }) => (
                      <Form className="mt-5 space-y-4">
                        <div>
                          <label htmlFor="email" className="block text-sm font-medium text-gray-700">
                            Email
                          </label>
                          <Field
                            type="email"
                            name="email"
                            id="email"
                            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                          />
                          {errors.email && touched.email && (
                            <div className="text-red-500 text-xs mt-1">{errors.email}</div>
                          )}
                        </div>

                        <div>
                          <label htmlFor="role" className="block text-sm font-medium text-gray-700">
                            Role
                          </label>
                          <Field
                            as="select"
                            name="role"
                            id="role"
                            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                          >
                            <option value="user">User</option>
                            <option value="admin">Admin</option>
                          </Field>
                          {errors.role && touched.role && (
                            <div className="text-red-500 text-xs mt-1">{errors.role}</div>
                          )}
                        </div>

                        <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                          <button
                            type="submit"
                            disabled={isLoading}
                            className="inline-flex w-full justify-center rounded-md bg-primary-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-primary-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary-600 sm:col-start-2"
                          >
                            {isLoading ? 'Sending...' : 'Send Invitation'}
                          </button>
                          <button
                            type="button"
                            className="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:col-start-1 sm:mt-0"
                            onClick={onClose}
                          >
                            Cancel
                          </button>
                        </div>
                      </Form>
                    )}
                  </Formik>
                </div>
              </Dialog.Panel>
            </Transition.Child>
          </div>
        </div>
      </Dialog>
    </Transition.Root>
  );
}